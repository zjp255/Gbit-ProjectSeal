using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const
{
    #region 蓄力跳跃
    public static int ANGER_MAX = 40;       // 蓄力最大值
    public static int ANGER_MIN = 0;        // 蓄力最小值
    public static int ANGER_UNIT = 10;      // 蓄力每10点1格
    public static List<float> ANGER_HEIGHT = new List<float>() { 0, 3, 3, 3, 3 };

    public static int BLOOD_MAX = 5;
    public static int BLOOD_MIN = 0;
    public static float DIRTY_MAX = 5f;
    public static float DIRTY_MIN = 0f;
    #endregion


}