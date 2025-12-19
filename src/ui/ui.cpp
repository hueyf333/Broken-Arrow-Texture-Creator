#include "ui/ui.hpp"

#include "core/core.hpp"
#include "render/render.hpp"
#include "tools/tools.hpp"

namespace batc::ui {

std::string build_title() {
    auto version = batc::core::version();
    auto normalized = batc::tools::normalize_name(version.name);
    auto frame = batc::render::describe_frame({ 1920, 1080 });
    return normalized + " :: " + frame;
}

} // namespace batc::ui
