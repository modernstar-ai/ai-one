import { getIndexes } from '@/services/indexes-service';
import { useEffect, useState } from 'react';

export function useIndexes() {
  const [indexes, setIndexes] = useState<string[] | undefined>(undefined);
  const [indexesLoading, setIndexesLoading] = useState<boolean>(indexes === undefined);

  useEffect(() => {
    setIndexesLoading(true);

    getIndexes()
      .then(setIndexes)
      .finally(() => setIndexesLoading(false));
  }, []);

  return { indexes, indexesLoading };
}
