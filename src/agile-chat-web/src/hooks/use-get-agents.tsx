import { useEffect, useState, useCallback } from 'react';
import { fetchAllAgents } from '@/services/agents-service';

const useGetAgents = () => {
  const [data, setData] = useState<any[] | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const fetchAgents = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetchAllAgents();
      setData(response);
    } catch (err) {
      setError(err as Error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAgents();
  }, [fetchAgents]);

  return {
    data,
    error,
    isLoading,
    refetch: fetchAgents
  };
};

export default useGetAgents;