#include "vulkan/VulkanCommandContext.h"

#include <stdexcept>

#include "vulkan/VulkanRenderDevice.h"

namespace renderer::vulkan_backend {

namespace {
size_t BytesPerPixel(Format format) {
  switch (format) {
    case Format::kR8G8B8A8Unorm:
      return 4;
    case Format::kR16G16B16A16Float:
      return 8;
    case Format::kR32Float:
      return 4;
  }
  return 0;
}
}

VulkanCommandContext::VulkanCommandContext(VulkanRenderDevice& device, std::string name)
    : device_(device), name_(std::move(name)) {
  vk::CommandPoolCreateInfo pool_info{};
  pool_info.queueFamilyIndex = device_.compute_queue_family();
  pool_info.flags = vk::CommandPoolCreateFlagBits::eResetCommandBuffer;
  command_pool_ = device_.device().createCommandPool(pool_info);

  vk::CommandBufferAllocateInfo alloc_info{};
  alloc_info.commandPool = command_pool_;
  alloc_info.level = vk::CommandBufferLevel::ePrimary;
  alloc_info.commandBufferCount = 1;
  command_buffer_ = device_.device().allocateCommandBuffers(alloc_info).front();
}

VulkanCommandContext::~VulkanCommandContext() {
  if (command_buffer_) {
    device_.device().freeCommandBuffers(command_pool_, command_buffer_);
  }
  if (command_pool_) {
    device_.device().destroyCommandPool(command_pool_);
  }
}

void VulkanCommandContext::begin() {
  if (recording_) {
    throw std::runtime_error("Command buffer already recording");
  }
  vk::CommandBufferBeginInfo begin_info{};
  command_buffer_.begin(begin_info);
  recording_ = true;
}

void VulkanCommandContext::end() {
  if (!recording_) {
    throw std::runtime_error("Command buffer not recording");
  }
  command_buffer_.end();
  recording_ = false;
}

void VulkanCommandContext::uploadTexture(const Texture2D& texture, const void* data,
                                         size_t bytes) {
  (void)texture;
  (void)data;
  (void)bytes;
  // TODO: allocate a staging buffer (VMA), map + copy, then record buffer->image copy.
}

void VulkanCommandContext::dispatch(const ComputePipeline& pipeline, uint32_t group_x,
                                    uint32_t group_y, uint32_t group_z) {
  (void)pipeline;
  command_buffer_.dispatch(group_x, group_y, group_z);
}

std::vector<std::byte> VulkanCommandContext::readbackTexture(const Texture2D& texture) {
  size_t bytes = texture.extent().width * texture.extent().height * BytesPerPixel(texture.format());
  // TODO: allocate readback buffer + copy image -> buffer -> map + read.
  return std::vector<std::byte>(bytes);
}

}  // namespace renderer::vulkan_backend
