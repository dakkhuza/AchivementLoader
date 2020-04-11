using BepInEx;
using RoR2;
using RoR2.Achievements;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using BepInEx.Logging;
using Dak.AchievementLoader.CustomAchievement;

namespace Dak.AchievementLoader
{
    [BepInPlugin("com.dakkhuza.plugins.achievementloader", "AchievementLoader", "1.0.0")]
    public class AchievementLoader : BaseUnityPlugin
    {
		void Awake()
		{
			//Create the harmony instance and patch them methods
			var harmony = new Harmony("com.dakkhuza.patchers.achievementloader");
			harmony.PatchAll();
		}
	}

	[HarmonyPatch(typeof(UnlockableCatalog), "Init")]
	class PatchUnlockable
	{
		static bool Prefix(ref Dictionary<string, UnlockableDef> ___nameToDefTable)
		{
			//Set up logging
			var unlockableLogger = new ManualLogSource("AchievementLoader.Unlock");
			BepInEx.Logging.Logger.Sources.Add(unlockableLogger);
			unlockableLogger.LogMessage("__== Unlock Loader ==__");

			unlockableLogger.LogInfo("Searching for custom unlockables...");

			//Build a list of all CustomUnlocks
			var customUnlockableDefs =
			from a in AppDomain.CurrentDomain.GetAssemblies()
			from t in a.GetTypes()
			let attributes = t.GetCustomAttributes(typeof(CustomUnlockable), true)
			where attributes != null && attributes.Length > 0
			select(attributes.Cast<CustomUnlockable>().ToArray()[0]);

			if (customUnlockableDefs.Count() > 0)
			{
				unlockableLogger.LogInfo(string.Format("Found {0} custom unlock(s) to add to catalog", customUnlockableDefs.Count()));

				//Iterate through each custom unlock and add them to the unlock catalog

				foreach (CustomUnlockable customUnlock in customUnlockableDefs)
				{
					//Create new unlock def
					UnlockableDef newUnlock = customUnlock.GetUnlockableDef();

					//Set the index of the unlock def
					newUnlock.index = new UnlockableIndex(___nameToDefTable.Count);

					//Add the def to the unlock table
					___nameToDefTable.Add(newUnlock.name, newUnlock);

					unlockableLogger.LogDebug(string.Format("Added Custom Unlock {0}", newUnlock.name));
				}
			}else
			{
				unlockableLogger.LogInfo("Found no custom unlocks to add");
			}
			unlockableLogger.LogInfo("Done!");

			//Continue on the the original unlock catalog
			return true;
		}
	}

	[HarmonyPatch(typeof(AchievementManager), "CollectAchievementDefs")]
	class PatchManager
	{
		static bool Prefix(ref Dictionary<string, AchievementDef> ___achievementNamesToDefs, ref List<string> ___achievementIdentifiers, ref AchievementDef[] ___achievementDefs, ref AchievementDef[] ___serverAchievementDefs, ref Action ___onAchievementsRegistered, Dictionary<string, AchievementDef> map)
		{
			//Setup logging
			var achievementLogger = new ManualLogSource("AchievementLoader.Achievement");
			achievementLogger.LogMessage("__== Achievement Loader ==__");
			BepInEx.Logging.Logger.Sources.Add(achievementLogger);

			achievementLogger.LogInfo("Searching for overrides");

			//Search and collect overrides
			IEnumerable<Type> achievementOverrides = 
				from a in AppDomain.CurrentDomain.GetAssemblies()
				from t in a.GetTypes()
				let attributes = t.GetCustomAttributes(typeof(OverrideAchievement), true)
				where attributes != null && attributes.Length > 0
				select (attributes.Cast<OverrideAchievement>().ToArray()[0].achievementType);

			//Convert from IEnum to List since it's way faster to search a list lmao
			List<Type> achievementOverrideTypes = achievementOverrides.ToList();

			achievementLogger.LogInfo(string.Format("Found {0} overrides", achievementOverrides.Count()));

			//Used for building achievement unlock chains?
			List<AchievementDef> list = new List<AchievementDef>();

			//Used for building achievement unlock chains?
			map.Clear();

			//Get all classes that inherit from BaseAchievement
			foreach (Type achievementClass in from type in (AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic)
            .SelectMany(a => a.GetTypes()))
								   where type.IsSubclassOf(typeof(BaseAchievement))
								   orderby type.Name
								   select type)
			{
				//Check if the achievement has an override defined
				if (achievementOverrideTypes.Contains(achievementClass))
				{
					//If it does, don't add it
					continue;
				}

				//Get the achievement attribute
				RegisterAchievementAttribute registerAchievementAttribute = (RegisterAchievementAttribute)Attribute.GetCustomAttribute(achievementClass, typeof(RegisterAchievementAttribute));

				//Check if the achievement was registered
				if (registerAchievementAttribute != null)
				{
					//Check to see if the achievement was overrided


					//Make sure each achievement is unique
					if (map.ContainsKey(registerAchievementAttribute.identifier))
					{
						Debug.LogErrorFormat("Class {0} attempted to register as achievement {1}, but class {2} has already registered as that achievement.", new object[]
						{
					achievementClass.FullName,
					registerAchievementAttribute.identifier,
					___achievementNamesToDefs[registerAchievementAttribute.identifier].type.FullName
						});
                    }
					else
					{
						//If  the achievement is unique, build the achievement
						AchievementDef achievementDef2 = new AchievementDef
						{
							identifier = registerAchievementAttribute.identifier,
							unlockableRewardIdentifier = registerAchievementAttribute.unlockableRewardIdentifier,
							prerequisiteAchievementIdentifier = registerAchievementAttribute.prerequisiteAchievementIdentifier,
							nameToken = "ACHIEVEMENT_" + registerAchievementAttribute.identifier.ToUpper(CultureInfo.InvariantCulture) + "_NAME",
							descriptionToken = "ACHIEVEMENT_" + registerAchievementAttribute.identifier.ToUpper(CultureInfo.InvariantCulture) + "_DESCRIPTION",
							iconPath = "Textures/AchievementIcons/tex" + registerAchievementAttribute.identifier + "Icon",
							type = achievementClass,
							serverTrackerType = registerAchievementAttribute.serverTrackerType
						};
						//Add the achievement identifier to the achievement identifier list
						___achievementIdentifiers.Add(registerAchievementAttribute.identifier);

						//Used for building achievement unlock chains?
						map.Add(registerAchievementAttribute.identifier, achievementDef2);

						//Used to build achievement defs array?
						list.Add(achievementDef2);

						//Get the related unlockable def
						UnlockableDef unlockableDef = UnlockableCatalog.GetUnlockableDef(achievementDef2.unlockableRewardIdentifier);

						//If there is an unlockable def, set the how to unlock and unlocked text
						if (unlockableDef != null)
						{
							string achievementName = Language.GetString(achievementDef2.nameToken);
							string achievementDescription = Language.GetString(achievementDef2.descriptionToken);
							unlockableDef.getHowToUnlockString = (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
							{
								achievementName,
								achievementDescription
							}));

							unlockableDef.getUnlockedString = (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
							{
								achievementName,
								achievementDescription
							}));
						}
					}
				}else
				{
					//Warn that a class inheriting from BaseAchievement doesn't have a AchievementAttribute
					string msg = string.Format("Class {0} in {1} inherits from BaseAchievement but doesn't have a RegisterAchievement attribute.", new object[] { achievementClass.FullName, achievementClass.Assembly.GetName().Name });
					achievementLogger.LogWarning(msg);
				}
			}

			___achievementDefs = list.ToArray();

			//Order achievements
			___achievementDefs = ___achievementDefs.OrderBy(def => UnlockableCatalog.GetUnlockableSortScore(def.unlockableRewardIdentifier)).ToArray();

			//Get achievements that need to be server tracked
			___serverAchievementDefs = (from achievementDef in ___achievementDefs
														where achievementDef.serverTrackerType != null
														select achievementDef).ToArray<AchievementDef>();

			//Build achievements to be tracked by client?
			for (int i = 0; i < ___achievementDefs.Length; i++)
			{
				___achievementDefs[i].index = new AchievementIndex
				{
					intValue = i
				};
			}

			//Build achievements to be tracked by server?
			for (int j = 0; j < ___serverAchievementDefs.Length; j++)
			{
				___serverAchievementDefs[j].serverIndex = new ServerAchievementIndex
				{
					intValue = j
				};
			}

			//Build achievement requirement chains?
			for (int k = 0; k < ___achievementIdentifiers.Count; k++)
			{
				string currentAchievementIdentifier = ___achievementIdentifiers[k];
				map[currentAchievementIdentifier].childAchievementIdentifiers = (from v in ___achievementIdentifiers
																				 where map[v].prerequisiteAchievementIdentifier == currentAchievementIdentifier
																				 select v).ToArray<string>();
			}

			//Let the system know all achievements have been created
			Action action = ___onAchievementsRegistered;
			if (action == null)
			{
				return false;
			}
			action();

			//Skip the original code
			return false;
		}
	}
}
