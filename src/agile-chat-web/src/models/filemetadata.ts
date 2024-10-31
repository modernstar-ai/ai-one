// src/models/fileMetadata.ts

export interface FileMetadata {
    id: string;            // Cosmos DB identifier
    fileId: string;        // File identifier (GUID)
    fileName: string;      // Name of the file
    blobUrl: string;       // Blob storage URL (changed to string)
    contentType?: string;  // MIME type of the file
    size: number;          // File size in bytes
    folder?: string;       // Folder name (optional)
    submittedOn?: string;  // Submission date (optional)     // File processing state (optional)
  }
  