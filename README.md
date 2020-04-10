# Riks of Rain 2 Achievement Loader

A loader of sorts of the achievement variety

### Installing

Download the .dll from either [Thunderstore](https://thunderstore.io/) or from the [Releases](https://github.com/dakkhuza/AchivementLoader/releases) page and place it into your Bepinex plugins folder.

You'll know it's working if you see this in the output console
![Image of it working correctly](http://dakkhuza.com/images/Unlock.png)

### How to make a Custom Achievement
To make a custom achievement just follow these simple steps.

1. Add "AchievementLoader" as a dependency to your project
1. Add a class to your plugin that inherits from BaseAchievement
1. Decorate your class with the \[RegisterAchievement\] attribute
1. Override OnInstall and OnUninstall with whatever you need to check for your challenge
1. Call Grant() when the criteria is met
1. (Optionable) If you don't do these, the challenge will still work but text related to it will be broken and it won't unlock anything when completed
   1. Decorate your achievement class with a \[CustomUnlockable\] attribute
   1. Add tokens to the language file for your challenge in the format ACHIEVEMENT_NAMEINCAPS_NAME and ACHIEVEMENT_NAMEINCAPS_DESCRIPTION
   * The [AssetPlus](https://github.com/risk-of-thunder/R2API/wiki/AssetPlus) api makes this easy
   
Here's an example of what a valid achievement class looks like
```C#
  [CustomUnlockable("Example.Example", "ACHIEVEMENT_EXAMPLEACHIEVEMENT_DESCRIPTION")]
  [RegisterAchievement("ExampleAchievement", "Example.Example", null, null)]
  public class ExampleAchievement : BaseAchievement
  {
      public override void OnInstall()
      {
          base.OnInstall();
          RoR2Application.onUpdate += AutoGrant;
      }

      public override void OnUninstall()
      {
          base.OnUninstall();
          RoR2Application.onUpdate -= AutoGrant;
      }

      public void AutoGrant()
      {
          Grant();
      }
  }
```
For more details check out the [wiki](https://github.com/dakkhuza/AchivementLoader/wiki)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* I'd like to thank the Harmony devs for making this so straight forward
