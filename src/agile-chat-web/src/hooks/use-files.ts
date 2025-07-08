import { useEffect, useState } from 'react';
import { getFiles } from '@/services/files-service';
import { CosmosFile } from '@/models/filemetadata';
import { PagedResultsDto } from '@/models/pagedResultsDto';
import { QueryDto } from '@/models/querydto';

export const useFetchFiles = () => {
  const [queryDto, setQueryDto] = useState<QueryDto>({ page: 0, pageSize: 10 } as QueryDto);
  const [files, setFiles] = useState<PagedResultsDto<CosmosFile>>({ page: 0, pageSize: 10, totalCount: 0, items: [] });
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<Error | null>(null);

  // Function to fetch files
  const fetchFiles = async () => {
    setLoading(true);
    setError(null); // Reset any previous error
    try {
      const fetchedFiles = await getFiles(queryDto);
      setFiles(fetchedFiles);
    } catch (err) {
      setError(err as Error);
    } finally {
      setLoading(false);
    }
  };

  // Initial fetch when the component mounts
  useEffect(() => {
    fetchFiles();
  }, [queryDto]);

  // Return the data, loading state, error state, and refetch function
  return {
    files,
    loading,
    error,
    queryDto,
    setQueryDto,
    refetch: fetchFiles // Refetch function to manually refresh data
  };
};
