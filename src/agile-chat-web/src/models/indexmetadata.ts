import { PermissionsAccessControlModel } from '@/components/ui-extended/permissions-access-control';

export type Index = {
  id: string;
  name: string;
  description?: string;
  chunkSize: number;
  chunkOverlap: number;
  accessControl: PermissionsAccessControlModel;
  createdAt: string;
  createdBy: string;
};

export type CreateIndexDto = {
  name: string;
  description?: string;
  chunkSize: number;
  chunkOverlap: number;
  accessControl: PermissionsAccessControlModel;
};

export interface IndexReportDto {
  searchIndexStatistics?: SearchIndexStatistics;
}

export interface SearchIndexStatistics {
  name?: string;
  documentCount?: number;
  storageSize?: string;
  vectorIndexSize?: string;
  replicasCount?: number;
  lastRefreshTime?: string;
  status?: string;
}
