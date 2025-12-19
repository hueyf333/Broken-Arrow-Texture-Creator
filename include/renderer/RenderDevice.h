#pragma once

#include <cstddef>
#include <memory>
#include <string>
#include <vector>

#include "renderer/CommandContext.h"
#include "renderer/Pipeline.h"
#include "renderer/Resource.h"

namespace renderer {

struct DeviceInfo {
  std::string name;
  uint32_t api_version = 0;
};

class RenderDevice {
 public:
  virtual ~RenderDevice() = default;

  virtual DeviceInfo info() const = 0;

  virtual CommandContextPtr createCommandContext(std::string name) = 0;
  virtual std::unique_ptr<Texture2D> createTexture2D(Extent2D extent, Format format,
                                                     std::string name) = 0;
  virtual std::unique_ptr<Buffer> createBuffer(size_t size_bytes, std::string name) = 0;
  virtual std::unique_ptr<ComputePipeline> createComputePipeline(std::string name,
                                                                 const std::vector<uint32_t>&
                                                                     spirv_bytecode) = 0;

  virtual void submit(CommandContext& context) = 0;
  virtual void waitIdle() = 0;
};

using RenderDevicePtr = std::unique_ptr<RenderDevice>;

}  // namespace renderer
