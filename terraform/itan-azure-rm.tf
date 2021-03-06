provider "azurerm" {
  version = "=2.32"
  subscription_id = var.subscription-id
  tenant_id = var.tenant-id
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
}

