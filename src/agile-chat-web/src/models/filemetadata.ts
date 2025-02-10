// src/models/fileMetadata.ts

export interface CosmosFile {
  id: string;
  name: string;
  url: string;
  blobUrl: string;
  contentType?: string;
  size: number;
  folderName?: string;
  indexName: string;
  createdDate: string;
  lastModified: string;
}
