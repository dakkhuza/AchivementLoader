using JetBrains.Annotations;
using RoR2;
using RoR2.Achievements;
using System;

namespace Dak.AchievementLoader.CustomAchievement
{
	/// <summary>
	/// Adds a custom unlock to Risk of Rain 2's unlock catalog
	/// </summary>
	public class CustomUnlockable : Attribute
	{
		public string name;
		public string nameToken;

		/// <summary>
		/// Gets an UnlockableDef from the attributes properties
		/// </summary>
		public UnlockableDef GetUnlockableDef()
		{
			return new UnlockableDef() { name = name, nameToken = nameToken };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The identification name</param>
		/// <param name="nameToken">The token used to seach up the unlock condition?</param>
		public CustomUnlockable(string name, string nameToken)
		{
			this.name = name;
			this.nameToken = nameToken;
		}
	}

	/// <summary>
	/// Overrides the specified achievement
	/// </summary>
	public class OverrideAchievement : Attribute
	{
		public Type achievementType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="achievementType">The achievement to override</param>
		public OverrideAchievement(Type achievementType)
		{
			this.achievementType = achievementType;
		}
	}

	public abstract class CustomAchievementAttribute : RegisterAchievementAttribute
	{
		/// <summary>
		/// The provider prefix for cohesion with R2Api
		/// </summary>
		public abstract string ProviderPrefix { get; }

		/// <summary>
		/// The path to where your icon is located in your asset bundle
		/// </summary>
		public virtual string Path => "Assets/AchievementIcons/";

		/// <summary>
		/// The extension of the icon
		/// </summary>
		public virtual string Extension => ".png";

		/// <summary>
		/// Returns the full path of the icon
		/// </summary>
		/// <param name="iconName"></param>
		/// <returns></returns>
		public string GetPath(string iconName)
		{
			return ProviderPrefix + Path + iconName + Extension;
		}
		public CustomAchievementAttribute([NotNull] string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier, Type serverTrackerType = null) : base(identifier, unlockableRewardIdentifier, prerequisiteAchievementIdentifier, serverTrackerType)
		{
		}
	}
}
