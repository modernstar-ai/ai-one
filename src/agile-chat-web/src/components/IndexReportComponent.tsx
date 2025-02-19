import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Loader2,
  AlertCircle,
  RefreshCcw,
  Database,
  Clock,
  Files,
  FileText,
  HardDrive,
  BarChart,
  Activity,
  LucideIcon
} from 'lucide-react';
import { toast } from '@/components/ui/use-toast';
import { IndexReportDto } from '@/models/indexmetadata';
import { getIndexReport } from '@/services/indexes-service';

const getBadgeVariant = (status: string | number | undefined | null) => {
  if (!status) return 'outline';
  switch (status.toString().toLowerCase()) {
    case 'active':
    case 'connected':
    case 'running':
    case 'enabled':
      return 'success';
    case 'error':
    case 'failed':
    case 'disconnected':
      return 'destructive';
    case 'warning':
      return 'default';
    case 'processing':
    case 'pending':
      return 'secondary';
    default:
      return 'outline';
  }
};

const formatFileSize = (bytesStr: string | number | null): string => {
  // Handle null, undefined, or empty string
  if (!bytesStr) return '0 B';

  // Handle string input
  const trimmedStr = String(bytesStr).trim();
  if (trimmedStr === '') return '0 B';

  // Try to parse the number, handling scientific notation
  let bytes: number;
  try {
    bytes = typeof bytesStr === 'number' ? bytesStr : Number(trimmedStr);
  } catch {
    return '0 B';
  }

  // Handle invalid numbers
  if (isNaN(bytes) || bytes < 0) return '0 B';
  if (bytes === 0) return '0 B';

  // Define size units
  const sizes: string[] = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];

  // Find the appropriate unit
  const order = Math.floor(Math.log(bytes) / Math.log(1024));

  // Handle numbers too large for our units
  if (order >= sizes.length) {
    return 'Size too large';
  }

  // Calculate the final size
  const size = bytes / Math.pow(1024, order);

  // Format with appropriate decimal places
  // Use 0 decimals for bytes, up to 2 for larger units
  const decimals = order === 0 ? 0 : 2;
  return `${size.toFixed(decimals).replace(/\.?0+$/, '')} ${sizes[order]}`;
};

interface StatsCardProps {
  icon: LucideIcon;
  title: string;
  value: string | number | Date | undefined | null;
  className?: string;
}

const StatsCard: React.FC<StatsCardProps> = ({ icon: Icon, title, value, className = '' }) => (
  <Card className={`${className} hover:shadow-lg transition-shadow duration-200`}>
    <CardContent className="p-6">
      <div className="flex items-center space-x-4">
        <div className="p-3 bg-primary/10 rounded-full">
          <Icon className="h-6 w-6 text-primary" />
        </div>
        <div>
          <p className="text-sm font-medium text-muted-foreground">{title}</p>
          <h3 className="text-xl font-bold mt-1">{value?.toString()}</h3>
        </div>
      </div>
    </CardContent>
  </Card>
);

const StatusCard: React.FC<StatsCardProps> = ({ icon: Icon, title, value, className = '' }) => (
  <Card className={`${className} hover:shadow-lg transition-shadow duration-200`}>
    <CardContent className="p-6">
      <div className="flex items-center space-x-4">
        <div className="p-3 bg-primary/10 rounded-full">
          <Icon className="h-6 w-6 text-primary" />
        </div>
        <div>
          <p className="text-sm font-medium text-muted-foreground">{title}</p>
          <h3 className="text-xl font-bold mt-1">
            <Badge variant={getBadgeVariant(value?.toString())} className="animate-fade-in">
              {value?.toString() || 'Unknown'}
            </Badge>
          </h3>
        </div>
      </div>
    </CardContent>
  </Card>
);

const IndexReportComponent: React.FC = () => {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [report, setReport] = useState<IndexReportDto | null>(null);
  const [searchParams] = useSearchParams();
  const indexName = searchParams.get('indexname');

  const loadReport = async () => {
    if (!indexName) {
      setError('Index name is required');
      setIsLoading(false);
      return;
    }

    try {
      setError(null);

      const response = await getIndexReport(indexName);
      if (!response) throw new Error('Failed to fetch Index Report');
      setReport(response);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to load index report';
      console.error('Error loading report:', error);
      setError(errorMessage);
      toast({
        title: 'Error',
        description: errorMessage,
        variant: 'destructive'
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadReport();
  }, [indexName]);

  if (isLoading) {
    return (
      <Card>
        <CardContent className="flex items-center justify-center h-64">
          <div className="text-center">
            <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
            <p className="text-muted-foreground">Loading report data...</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error || !report) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center h-64 text-center">
          <AlertCircle className="h-8 w-8 text-destructive mb-4" />
          <p className="text-lg font-medium mb-2">{error || 'No report data available'}</p>
          <button
            onClick={loadReport}
            className="flex items-center space-x-2 px-4 py-2 rounded-md bg-primary text-primary-foreground hover:bg-primary/90 transition-colors">
            <RefreshCcw className="h-4 w-4" />
            <span>Try Again</span>
          </button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-8">
      {/* Stats Overview */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatsCard icon={FileText} title="Name" value={report.searchIndexStatistics?.name || indexName} />
        <StatsCard
          icon={Files}
          title="Total Documents"
          value={report.searchIndexStatistics?.documentCount?.toLocaleString() || 'N/A'}
        />
        <StatsCard
          icon={HardDrive}
          title="Storage Size"
          value={
            report?.searchIndexStatistics?.storageSize
              ? formatFileSize(report.searchIndexStatistics.storageSize)
              : 'N/A'
          }
        />
        <StatsCard
          icon={Database}
          title="Vector Index Size"
          value={
            report.searchIndexStatistics?.vectorIndexSize
              ? formatFileSize(report.searchIndexStatistics.vectorIndexSize)
              : 'N/A'
          }
        />
        <StatsCard
          icon={Clock}
          title="Last Refresh"
          value={report.searchIndexStatistics?.lastRefreshTime || report.indexer?.lastRunTime || 'N/A'}
        />
        <StatsCard icon={BarChart} title="Replicas Count" value={report.searchIndexStatistics?.replicasCount || '1'} />
        <StatusCard icon={Activity} title="Status" value={report.searchIndexStatistics?.status || 'Active'} />
      </div>
    </div>
  );
};

export default IndexReportComponent;
