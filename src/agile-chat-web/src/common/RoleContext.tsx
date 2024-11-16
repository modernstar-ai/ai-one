import React, { createContext, useContext, useEffect, useState } from 'react';
import { IsUserSystemAdmin, IsUserContentManager, IsUserEndUser } from '@/services/custom-role-service';

// Define the shape of the RoleContext
interface RoleContextProps {
  isSystemAdmin: boolean;
  isContentManager: boolean;
  isEndUser: boolean;
  loading: boolean;
}

// Initialize the context with a default value
const RoleContext = createContext<RoleContextProps | undefined>(undefined);

// RoleContext provider component
export const RoleProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isSystemAdmin, setIsSystemAdmin] = useState(false);
  const [isContentManager, setIsContentManager] = useState(false);
  const [isEndUser, setIsEndUser] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Fetch the role data when the component mounts
    const fetchRoles = async () => {
      setLoading(true);
      const email =  import.meta.env.VITE_USER_EMAIL as string || ''; // Use your env variable for email
      try {
        const systemAdmin = await IsUserSystemAdmin(email);
        const contentManager = await IsUserContentManager(email);
        const endUser = await IsUserEndUser(email);
        setIsSystemAdmin(systemAdmin);
        setIsContentManager(contentManager);
        setIsEndUser(endUser);
      } catch (error) {
        console.error("Error fetching roles:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchRoles();
  }, []);

  return (
    <RoleContext.Provider value={{ isSystemAdmin, isContentManager, isEndUser, loading }}>
      {children}
    </RoleContext.Provider>
  );
};

// Custom hook to use RoleContext in other components
export const useRoleContext = () => {
  const context = useContext(RoleContext);
  if (context === undefined) {
    throw new Error('useRoleContext must be used within a RoleProvider');
  }
  return context;
};