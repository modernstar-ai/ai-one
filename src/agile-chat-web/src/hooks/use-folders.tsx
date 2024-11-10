import { getFolders } from '@/services/folders-service';
import { useEffect, useState } from 'react';

export function useFolders() {
  const [folders, setFolders] = useState<string[] | undefined>(undefined);
  const [foldersLoading, setFoldersLoading] = useState<boolean>(folders === undefined);

  useEffect(() => {
    setFoldersLoading(true);

    getFolders()
      .then(setFolders)
      .finally(() => setFoldersLoading(false));
  }, []);

  return { folders, foldersLoading };
}
