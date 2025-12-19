#pragma once

#include <cstddef>
#include <cstdint>
#include <string>

namespace renderer {

enum class ResourceType {
  kBuffer,
  kTexture2D
};

enum class Format {
  kR8G8B8A8Unorm,
  kR16G16B16A16Float,
  kR32Float
};

struct Extent2D {
  uint32_t width = 0;
  uint32_t height = 0;
};

class Resource {
 public:
  Resource(ResourceType type, std::string name) : type_(type), name_(std::move(name)) {}
  virtual ~Resource() = default;

  ResourceType type() const { return type_; }
  const std::string& name() const { return name_; }

 private:
  ResourceType type_{};
  std::string name_;
};

class Buffer : public Resource {
 public:
  Buffer(size_t size_bytes, std::string name)
      : Resource(ResourceType::kBuffer, std::move(name)), size_bytes_(size_bytes) {}

  size_t size_bytes() const { return size_bytes_; }

 private:
  size_t size_bytes_{};
};

class Texture2D : public Resource {
 public:
  Texture2D(Extent2D extent, Format format, std::string name)
      : Resource(ResourceType::kTexture2D, std::move(name)), extent_(extent), format_(format) {}

  Extent2D extent() const { return extent_; }
  Format format() const { return format_; }

 private:
  Extent2D extent_{};
  Format format_{};
};

}  // namespace renderer
