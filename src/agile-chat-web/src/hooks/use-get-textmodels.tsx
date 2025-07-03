import { useEffect, useState, useCallback } from 'react';
import { getTextModels } from '@/services/chat-completions-service';
import { IModelOptions } from '@/types/Assistant';

const useGetTextModels = () => {
  const [data, setData] = useState<IModelOptions | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);

  const fetchTextModels = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await getTextModels();
      setData(response);
    } catch (err) {
      setError(err as Error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTextModels();
  }, [fetchTextModels]);

  return {
    data,
    error,
    isLoading,
    refetch: fetchTextModels
  };
};

export default useGetTextModels;
