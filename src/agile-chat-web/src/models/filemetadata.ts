// src/models/fileMetadata.ts

export interface CosmosFile {
  id: string;
  name: string;
  url: string;
  blobUrl: string;
  contentType?: string;
  size: number;
  folder?: string;
  indexName: string;
  createdDate: string;
  lastModified: string;
}
