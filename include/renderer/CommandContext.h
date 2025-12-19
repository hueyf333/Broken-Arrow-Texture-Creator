#pragma once

#include <cstddef>
#include <cstdint>
#include <memory>
#include <vector>

#include "renderer/Pipeline.h"
#include "renderer/Resource.h"

namespace renderer {

class CommandContext {
 public:
  virtual ~CommandContext() = default;

  virtual void begin() = 0;
  virtual void end() = 0;

  virtual void uploadTexture(const Texture2D& texture, const void* data, size_t bytes) = 0;
  virtual void dispatch(const ComputePipeline& pipeline, uint32_t group_x, uint32_t group_y,
                        uint32_t group_z) = 0;
  virtual std::vector<std::byte> readbackTexture(const Texture2D& texture) = 0;
};

using CommandContextPtr = std::unique_ptr<CommandContext>;

}  // namespace renderer
