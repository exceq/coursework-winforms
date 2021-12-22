namespace server_connect
{
    static class ConfiguratePositionData
    {
        public const int JOINT_STRING_OFFSET = 10;
        public const int FLANGE_STRING_OFFSET = 14;
        public const int TOOL_STRING_OFFSET = 12;
        public const int CAMERA_STRING_OFFSET = 14;
        public const int BYTES_PER_KRC_REAL = 4;
        public const int DEGREES_OF_FREEDOM_NUM = 6;
        public const int AXIS_NUM = 6;

        public const int TCP_X_OFFSET = 12;
        public const int TCP_Y_OFFSET = 16;
        public const int TCP_Z_OFFSET = 20;
        public const int TCP_AROUND_Z_OFFSET = 24;
        public const int TCP_AROUND_Y_OFFSET = 28;
        public const int TCP_AROUND_X_OFFSET = 32;

        public const int CAMERA_X_OFFSET = 14;
        public const int CAMERA_Y_OFFSET = 18;
        public const int CAMERA_Z_OFFSET = 22;
        public const int CAMERA_AROUND_Z_OFFSET = 26;
        public const int CAMERA_AROUND_Y_OFFSET = 30;
        public const int CAMERA_AROUND_X_OFFSET = 34;

        public const int FLANGE_X_OFFSET = 14;
        public const int FLANGE_Y_OFFSET = 18;
        public const int FLANGE_Z_OFFSET = 22;
        public const int FLANGE_AROUND_Z_OFFSET = 26;
        public const int FLANGE_AROUND_Y_OFFSET = 30;
        public const int FLANGE_AROUND_X_OFFSET = 34;

        public const int RANGE_FINDER_X_OFFSET = 19;
        public const int RANGE_FINDER_Y_OFFSET = 23;
        public const int RANGE_FINDER_Z_OFFSET = 27;
        public const int RANGE_FINDER_AROUND_Z_OFFSET = 31;
        public const int RANGE_FINDER_AROUND_Y_OFFSET = 35;
        public const int RANGE_FINDER_AROUND_X_OFFSET = 39;

        public const string FLANGE_POS_MSG = "FlangePosition";
        public const string JOINT_STATE_MSG = "JointState";
        public const string TOOL_POS_MSG = "ToolPosition";
        public const string TOOL_AND_BASES_MSG = "ToolAndBases";
        public const string CAMERA_POSITION_MSG = "CameraPosition";
        public const string RANGE_FINDER_POS_MSG = "RangefinderPosition";
        public const string RANGE_FINDER_DISTANCE_MSG = "RangefinderDistance";
        public const int port_robot = 54600;
        public const string ip_robot = "192.168.7.107";

        public static string endString = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";
    }
}
