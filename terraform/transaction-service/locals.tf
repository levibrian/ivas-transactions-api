locals {
  transactions_resource_base_name = "${var.client}-${local.environment}-transactions"
  transactions_api_gateway_name   = "${local.transactions_resource_base_name}-api"
  transactions_lambda_name        = "${local.transactions_resource_base_name}-lambda"
  transactions_db_name            = "${local.transactions_resource_base_name}-database"
  transactions_lambda_file_path   = "${var.packages_path}/${var.package_file_name}"
  transactions_subdomain_name     = "${local.transactions_resource_base_name}-api-subdomain-name"
  environment                     = substr(terraform.workspace, 0, 3)
  logs_retention_in_days          = 14
  default_tags = {
    Stage        = local.environment
    Client       = var.client
    Service      = var.service_name
    ServiceGroup = local.transactions_resource_base_name
    Version      = var.version_tag
  }
}