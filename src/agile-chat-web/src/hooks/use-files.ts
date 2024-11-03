import { useEffect, useState } from 'react';
import { getFiles } from '@/services/cosmosservice';
import { FileMetadata } from '@/models/filemetadata';

  export const useFetchFiles = () => {
    const [files, setFiles] = useState<FileMetadata[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
  
    useEffect(() => {
      const fetchFiles = async () => {
        setLoading(true);
        try {
          console.log('Fetching files from backend...');
          const data = await getFiles(); // This now returns FileMetadata[]
          setFiles(data); // TypeScript now knows that `data` is FileMetadata[]
          setError(null);
        } catch (err: unknown) {
          console.error('Error occurred while fetching files:', err);
          setError('Failed to fetch files. Please try again later.');
        } finally {
          setLoading(false);
        }
      };
  
      fetchFiles();
    }, []);
  
    return { files, loading, error };
  };