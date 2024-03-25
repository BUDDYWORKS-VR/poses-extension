# BUDDYWORKS Poses Extension for GoGo Loco

A neat extension for GogoLoco with plenty of poses for photography.

The Poses are all put into a singular animation clip and dialed in via a radial menu. This not only makes it way easier to maintain, it also allows you to easily add additional poses. Note that the more poses you have, the more difficult it will get to dial them in with the radial menu. The limit is probably 100.

It is built using GogoLocos Action Layer, so GogoLoco is a hard dependency.

Also, the Pose System can only work in the VRCFury variant of GogoLoco.

If you want to install it to a baked GogoLoco Setup, remove the "Go/" from all Menu Parameters and use the provided Action Layer on your Avatar. If you made changes to your action layer, you might need to port them over to the provided controller.

Dependencies
- GogoLoco from Franada, in the correct Version

- VRCFury

Installation
1. Edit your GogoLoco Beyond (VRCFury) Prefab to use the supplied Action Layer instead of the native one.

2. Embed the PosesMain Menu into your Avatar wherever.

That should be it, upload and use.

Usage
Toggle the type of pose, and use the dial to select the desired pose.

Credit and ToS
Personal use only, do not redistribute.

Add more poses
The system comes with a couple example poses, mostly standing ones.

You can add more by editing the master clips in /Assets/BUDDYWORKS/Pose System/Poses/

Copy your animation keyframes at the end of the correct master clip, done!

How to port it to a different GogoLoco Version yourself
If you depend on an unsupported version of GogoLoco, you can port this system to work with your setup. This requieres that you know a bit about Animators.

1. Make a copy of your GogoLoco Action Layer your Avatar shall use.

2. Open that copy, double click "Standing Emotes"

3. Double Click "17-24".

4. Check that states 17-24 are empty or use GogoLocos T-Pose animation. If they are not, you might need to move up in the range until you find a free space.

5. For 17 to 21, Change Speed to 1, disable Multiplier Parameter and Enable Motion Time Parameter. Set Motion Time Parameter to Go/Float.

6. Slot in the following animations into Slot 17-21: Standing_Master, Sitting_Master, Kneel_Master, Prone_Master, Other_Master respectively.

6. Optional for Dances: Slot into 22-24 "Ch4nge_by_Evendora", "HipSwayV3_by_Evendora" and "VRSuya_Loli_Kami_Requiem" respectively.

Done, you have essentially the same setup now, move your Action Controller into the Pose System Folder and proceed like in "Installation", or use it in your descriptor if baked.

Need Help?
https://discord.buddyworks.wtf

Notes
The avatar used in the imagery is not included.
