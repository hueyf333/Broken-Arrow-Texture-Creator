#pragma once

#include <string>
#include <vector>

#include <vulkan/vulkan.hpp>

#include "renderer/CommandContext.h"
#include "renderer/Pipeline.h"
#include "renderer/Resource.h"

namespace renderer::vulkan_backend {

class VulkanRenderDevice;

class VulkanCommandContext final : public CommandContext {
 public:
  VulkanCommandContext(VulkanRenderDevice& device, std::string name);
  ~VulkanCommandContext() override;

  void begin() override;
  void end() override;

  void uploadTexture(const Texture2D& texture, const void* data, size_t bytes) override;
  void dispatch(const ComputePipeline& pipeline, uint32_t group_x, uint32_t group_y,
                uint32_t group_z) override;
  std::vector<std::byte> readbackTexture(const Texture2D& texture) override;

  vk::CommandBuffer command_buffer() const { return command_buffer_; }
  const std::string& name() const { return name_; }

 private:
  VulkanRenderDevice& device_;
  std::string name_;
  vk::CommandPool command_pool_{};
  vk::CommandBuffer command_buffer_{};
  bool recording_ = false;
};

}  // namespace renderer::vulkan_backend
