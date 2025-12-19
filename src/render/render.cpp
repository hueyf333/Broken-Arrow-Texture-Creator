#include "render/render.hpp"

#include <string>

namespace batc::render {

std::string describe_frame(const FrameInfo& frame) {
    return "Frame " + std::to_string(frame.width) + "x" + std::to_string(frame.height);
}

} // namespace batc::render
