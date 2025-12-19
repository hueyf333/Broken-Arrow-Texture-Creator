#pragma once

#include <string>

namespace renderer {

class Pipeline {
 public:
  explicit Pipeline(std::string name) : name_(std::move(name)) {}
  virtual ~Pipeline() = default;

  const std::string& name() const { return name_; }

 private:
  std::string name_;
};

class ComputePipeline : public Pipeline {
 public:
  using Pipeline::Pipeline;
};

}  // namespace renderer
