﻿using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class TiltBodyLeftCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            return false;
        }

        public override void Execute() {
            RoboManagerInstance.TiltBodyLeft();
        }
    }
}