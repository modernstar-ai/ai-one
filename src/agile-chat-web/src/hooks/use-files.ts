import { useEffect, useState, useCallback } from 'react';
import { getFiles } from '@/services/cosmosservice';
import { FileMetadata } from '@/models/filemetadata';

export const useFetchFiles = () => {
  const [files, setFiles] = useState<FileMetadata[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<Error | null>(null);

  // Function to fetch files
  const fetchFiles = useCallback(async () => {
    setLoading(true);
    setError(null); // Reset any previous error
    try {
      const fetchedFiles = await getFiles();
      setFiles(fetchedFiles);
    } catch (err) {
      setError(err as Error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial fetch when the component mounts
  useEffect(() => {
    fetchFiles();
  }, [fetchFiles]);

  // Return the data, loading state, error state, and refetch function
  return {
    files,
    loading,
    error,
    refetch: fetchFiles, // Refetch function to manually refresh data
  };
};