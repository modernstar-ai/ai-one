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
  status: FileStatus;
}

export enum FileStatus {
  Uploaded = 'Uploaded',
  QueuedForIndexing = 'QueuedForIndexing',
  Indexing = 'Indexing',
  Indexed = 'Indexed',
  QueuedForDeletion = 'QueuedForDeletion',
  Deleting = 'Deleting',
  Failed = 'Failed'
}
