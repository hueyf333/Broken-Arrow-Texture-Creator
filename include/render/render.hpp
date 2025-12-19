#pragma once

#include <string>

namespace batc::render {

struct FrameInfo {
    int width;
    int height;
};

std::string describe_frame(const FrameInfo& frame);

} // namespace batc::render
