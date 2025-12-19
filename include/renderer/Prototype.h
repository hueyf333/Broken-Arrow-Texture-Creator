#pragma once

#include <cstddef>
#include <cstdint>
#include <vector>

#include "renderer/RenderDevice.h"

namespace renderer::prototype {

struct TextureFlowResult {
  std::vector<std::byte> readback_bytes;
};

TextureFlowResult runTextureUploadComputeReadback(RenderDevice& device, Extent2D extent,
                                                  const std::vector<std::byte>& initial_pixels,
                                                  const std::vector<uint32_t>& compute_spirv);

}  // namespace renderer::prototype
