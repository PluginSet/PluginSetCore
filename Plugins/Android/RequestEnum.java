package com.pluginset.core;

public class RequestEnum {
    private static int NextStartCode = 0;

    public static RequestEnum Counter(int step)
    {
        RequestEnum counter = new RequestEnum(NextStartCode);
        NextStartCode = NextStartCode + step;
        return counter;
    }

    private int nextCode;

    private RequestEnum(int startCode)
    {
        nextCode = startCode;
    }

    public int Next()
    {
        return nextCode++;
    }
}
