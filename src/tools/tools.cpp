#include "tools/tools.hpp"

#include <algorithm>
#include <cctype>

namespace batc::tools {

std::string normalize_name(std::string_view name) {
    std::string result(name);
    std::transform(result.begin(), result.end(), result.begin(), [](unsigned char value) {
        return static_cast<char>(std::tolower(value));
    });
    return result;
}

} // namespace batc::tools
