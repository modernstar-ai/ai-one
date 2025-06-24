# Deployment Overview

This section provides a high-level overview of the AI-One deployment process, architecture, and prerequisites. Use this as your starting point before following the detailed guides for app registration and deployment using GitHub Actions or Azure DevOps.

---

## Solution Components

The AI-One solution is composed of three main components:

- **Platform**: Core infrastructure and shared services (e.g., Key Vault, Storage, Service Bus, Cosmos DB, OpenAI, etc.)
- **API**: Backend services (Agile.Chat API)
- **Web App**: Frontend application for user interaction

---

## Infrastructure as Code (IaC)

All infrastructure is provisioned using modular Bicep templates, located in the `infra/` directory:

```plaintext
infra/
├── platform/   # Platform Bicep code & parameter files
├── backend/    # API app Bicep code & parameter files
├── frontend/   # Web app Bicep code & parameter files
├── scripts/    # Deployment and utility scripts
└── modules/    # Reusable Bicep modules (Key Vault, Cosmos DB, etc.)
```

---

## Supported Deployment Methods

You can deploy AI-One using either:
- **GitHub Actions** (recommended for most users)
- **Azure DevOps Pipelines**

Each method is documented in its own guide. Before starting, ensure you have completed the [App Registration](app_registration.md) steps.

---

Continue to the next guide for detailed instructions:
- [App Registration](app_registration.md)
- [GitHub Deployment Steps](github_steps.md)
- [Azure DevOps Deployment Steps](azure_devops_steps.md)
