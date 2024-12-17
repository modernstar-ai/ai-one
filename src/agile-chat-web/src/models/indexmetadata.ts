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


export interface IndexReport {
  name?: string;
  documentCount?: number;
  storageSize?: string;
  vectorIndexSize?: string;
  replicasCount?: number;
  lastRefreshTime?: string;
  status?: string;
  indexers?: IndexerDetail[];
  dataSources?: DataSourceDetail[];
}

export interface IndexerDetail {
  name?: string;
  targetIndex?: string;
  dataSource?: string;
  schedule?: string;
  lastRunTime?: string;
  nextRunTime?: string;
  documentsProcessed?: string;
  status?: string;
}

export interface DataSourceDetail {
  name?: string;
  type?: string;
  container?: string;
  connectionStatus?: string;
}