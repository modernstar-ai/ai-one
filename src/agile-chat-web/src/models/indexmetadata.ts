export type Index = {
  id: string;
  name: string;
  description?: string;
  group?: string;
  createdAt: string;
  createdBy: string;
};

export type CreateIndexDto = {
  name: string;
  description?: string;
  group?: string;
};


export interface IndexReportDto {
  
  searchIndexStatistics? : SearchIndexStatistics;
  indexer?: IndexerDetail;
  dataSource?: DataSourceDetail;
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

export interface IndexerDetail {
  name?: string;
  targetIndex?: string;
  dataSource?: string;
  schedule?: string;
  lastRunTime?: Date;
  documentsProcessed?: string;
  status?: string;
}

export interface DataSourceDetail {
  name?: string;
  type?: string;
  container?: string;
  connectionStatus?: string;
}