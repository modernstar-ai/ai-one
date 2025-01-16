import SimpleHeading from '@/components/Heading-Simple';
import IndexReportComponent from '@/components/IndexReportComponent';

export default function IndexReport() {
  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading Title="Container Information" Subtitle={'Details and metrics of container'} DocumentCount={0} />
        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
              <IndexReportComponent />
          </main>
        </div>
      </div>
    </div>
  );
}
