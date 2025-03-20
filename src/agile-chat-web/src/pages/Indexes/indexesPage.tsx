import { PermissionHandler } from '@/authentication/permission-handler/permission-handler';
import { UserRole } from '@/authentication/user-roles';
import SimpleHeading from '@/components/Heading-Simple';
import IndexerComponent from '@/components/IndexerComponent';
import { Button } from '@/components/ui/button';
import { useNavigate } from 'react-router-dom';

export default function IndexesPage() {
  const navigate = useNavigate();

  const handleNewContainer = () => {
    navigate('/container-form');
  };

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading Title="Containers" Subtitle={'Configure your Containers'} DocumentCount={0} />
        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
            <PermissionHandler roles={[UserRole.SystemAdmin]}>
              <Button className="bg-black text-white hover:bg-gray-800 h-12" onClick={handleNewContainer}>
                New Container
              </Button>
            </PermissionHandler>
            <IndexerComponent />
          </main>
        </div>
      </div>
    </div>
  );
}
