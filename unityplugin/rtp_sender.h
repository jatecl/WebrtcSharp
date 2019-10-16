#pragma once
#include "framework.h"

extern "C" {
	__declspec(dllexport) bool RtpSender_SetTrack(void* ptr, void* track);
}