#pragma once

#include <string>

namespace batc::core {

struct VersionInfo {
    std::string name;
    int major;
    int minor;
};

VersionInfo version();

} // namespace batc::core
