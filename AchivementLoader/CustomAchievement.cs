using RoR2;
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
		/// <param name="nameToken">The token used to seach up the unlock condition</param>
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
}
