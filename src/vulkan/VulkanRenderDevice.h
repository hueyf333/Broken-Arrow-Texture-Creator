#pragma once

#include <memory>
#include <string>
#include <vector>

#include <vulkan/vulkan.hpp>
#include <vk_mem_alloc.h>

#include "renderer/RenderDevice.h"

namespace renderer::vulkan_backend {

class VulkanCommandContext;

class VulkanRenderDevice final : public RenderDevice {
 public:
  VulkanRenderDevice();
  ~VulkanRenderDevice() override;

  DeviceInfo info() const override;

  CommandContextPtr createCommandContext(std::string name) override;
  std::unique_ptr<Texture2D> createTexture2D(Extent2D extent, Format format,
                                             std::string name) override;
  std::unique_ptr<Buffer> createBuffer(size_t size_bytes, std::string name) override;
  std::unique_ptr<ComputePipeline> createComputePipeline(std::string name,
                                                         const std::vector<uint32_t>&
                                                             spirv_bytecode) override;

  void submit(CommandContext& context) override;
  void waitIdle() override;

  vk::Device device() const { return device_.get(); }
  vk::Queue compute_queue() const { return compute_queue_; }
  uint32_t compute_queue_family() const { return compute_queue_family_index_; }
  VmaAllocator allocator() const { return allocator_; }

 private:
  void createInstance();
  void createDevice();
  void createAllocator();

  vk::UniqueInstance instance_;
  vk::PhysicalDevice physical_device_{};
  vk::UniqueDevice device_;
  vk::Queue compute_queue_{};
  uint32_t compute_queue_family_index_ = 0;
  VmaAllocator allocator_ = nullptr;
};

}  // namespace renderer::vulkan_backend
