#include "renderer/Prototype.h"

#include <stdexcept>

namespace renderer::prototype {

TextureFlowResult runTextureUploadComputeReadback(RenderDevice& device, Extent2D extent,
                                                  const std::vector<std::byte>& initial_pixels,
                                                  const std::vector<uint32_t>& compute_spirv) {
  if (initial_pixels.empty()) {
    throw std::runtime_error("Initial pixel buffer must not be empty");
  }

  auto texture = device.createTexture2D(extent, Format::kR8G8B8A8Unorm, "InputTexture");
  auto pipeline = device.createComputePipeline("ComputePipeline", compute_spirv);
  auto context = device.createCommandContext("TextureFlow");

  context->begin();
  context->uploadTexture(*texture, initial_pixels.data(), initial_pixels.size());
  context->dispatch(*pipeline, (extent.width + 7) / 8, (extent.height + 7) / 8, 1);
  auto readback = context->readbackTexture(*texture);
  context->end();

  device.submit(*context);
  device.waitIdle();

  return TextureFlowResult{std::move(readback)};
}

}  // namespace renderer::prototype
