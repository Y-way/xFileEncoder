﻿#include "XContext.h"
#include "common/Allocator.h"
namespace encrypt
{
    void XContext::Reset()
    {
        ReleaseData();
        _clone = false;
        _code = ResultCode::Unknown;
        _memoryType = XCodeMemoryType::None;
        _input = nullptr;
        _inputLength = 0;
    }

    void XContext::ReleaseData()
    {
        _length = 0;
        if (_type == XCodeMemoryType::AllocateMemory && _data != nullptr)
        {
            XMEMORY_FREE(_data);
            _data = nullptr;
        }
    }
}