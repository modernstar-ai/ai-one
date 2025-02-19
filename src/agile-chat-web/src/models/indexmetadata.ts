export type Index = {
  id: string;
  name: string;
  description?: string;
  chunkSize: number;
  chunkOverlap: number;
  group?: string;
  createdAt: string;
  createdBy: string;
};

export type CreateIndexDto = {
  name: string;
  description?: string;
  chunkSize: number;
  chunkOverlap: number;
  group?: string;
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
