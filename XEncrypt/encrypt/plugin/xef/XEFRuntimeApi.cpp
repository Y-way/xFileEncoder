﻿#include "XEFRuntimeApi.h"
#include "XEFPlugin.h"

using namespace xencrypt;

XENCRYPT_API void* xef_plugin_create(int type, uint8_t encryptSize)
{
    return new XEFPlugin((XEncodeType)type, encryptSize);
}

XENCRYPT_API void xef_plugin_destroy(void* plugin)
{
    XEFPlugin* pluginPtr = reinterpret_cast<XEFPlugin*>(plugin);
    if (pluginPtr == nullptr)
    {
        return;
    }
    delete pluginPtr;
    pluginPtr = nullptr;
}