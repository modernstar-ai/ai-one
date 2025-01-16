import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
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
    CircleDot,
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
    if (!bytesStr) return "0 B";
    
    // Handle string input
    const trimmedStr = String(bytesStr).trim();
    if (trimmedStr === "") return "0 B";
    
    // Try to parse the number, handling scientific notation
    let bytes: number;
    try {
        bytes = typeof bytesStr === 'number' ? bytesStr : Number(trimmedStr);
    } catch {
        return "0 B";
    }
    
    // Handle invalid numbers
    if (isNaN(bytes) || bytes < 0) return "0 B";
    if (bytes === 0) return "0 B";
    
    // Define size units
    const sizes: string[] = ["B", "KB", "MB", "GB", "TB", "PB"];
    
    // Find the appropriate unit
    const order = Math.floor(Math.log(bytes) / Math.log(1024));
    
    // Handle numbers too large for our units
    if (order >= sizes.length) {
        return "Size too large";
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
    value: string | number | undefined | null;
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
                    <h3 className="text-xl font-bold mt-1">{value}</h3>
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
                        <Badge variant={getBadgeVariant(value)}
                            className="animate-fade-in">
                            {value || 'Unknown'}
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
    const [refreshing, setRefreshing] = useState(false);
    const [searchParams] = useSearchParams();
    const indexName = searchParams.get('indexname');

    const loadReport = async () => {
        if (!indexName) {
            setError('Index name is required');
            setIsLoading(false);
            return;
        }

        try {
            setRefreshing(true);
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
                variant: 'destructive',
            });
        } finally {
            setIsLoading(false);
            setRefreshing(false);
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
                        className="flex items-center space-x-2 px-4 py-2 rounded-md bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
                    >
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
                <StatsCard
                    icon={FileText}
                    title="Name"
                    value={report.searchIndexStatistics?.name || indexName}
                />
                <StatsCard
                    icon={Files}
                    title="Total Documents"
                    value={report.searchIndexStatistics?.documentCount?.toLocaleString() || 'N/A'}
                    
                />
                <StatsCard
                    icon={HardDrive}
                    title="Storage Size"
                    value={report?.searchIndexStatistics?.storageSize ? formatFileSize(report.searchIndexStatistics.storageSize) : 'N/A'}
                />
                <StatsCard
                    icon={Database}
                    title="Vector Index Size"
                    value={report.searchIndexStatistics?.vectorIndexSize || 'N/A'}
                />
                <StatsCard
                    icon={Clock}
                    title="Last Refresh"
                    value={report.searchIndexStatistics?.lastRefreshTime || 'N/A'}
                />
                <StatsCard
                    icon={BarChart}
                    title="Replicas Count"
                    value={report.searchIndexStatistics?.replicasCount || '1'}
                />
                <StatusCard
                    icon={Activity}
                    title="Status"
                    value={report.searchIndexStatistics?.status || 'Active'}
                />
            </div>

            {/* Indexers Section */}
            <Card className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="flex flex-row items-center justify-between">
                    <CardTitle className="text-lg font-medium">Available Indexers</CardTitle>
                    <button
                        onClick={loadReport}
                        className="p-2 hover:bg-primary/10 rounded-full transition-colors"
                        disabled={refreshing}
                    >
                        <RefreshCcw className={`h-5 w-5 ${refreshing ? 'animate-spin' : ''}`} />
                    </button>
                </CardHeader>
                <CardContent>
                    <div className="overflow-x-auto">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Name</TableHead>
                                    <TableHead>Target Index</TableHead>
                                    <TableHead>Data Source</TableHead>
                                    <TableHead>Schedule</TableHead>
                                    <TableHead>Last Run</TableHead>
                                    <TableHead>Next Run</TableHead>
                                    <TableHead>Documents</TableHead>
                                    <TableHead>Status</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {report.indexers?.map((indexer) => (
                                    <TableRow key={indexer.name} className="hover:bg-muted/50 transition-colors">
                                        <TableCell className="font-medium">{indexer.name || 'N/A'}</TableCell>
                                        <TableCell>{indexer.targetIndex || 'N/A'}</TableCell>
                                        <TableCell>{indexer.dataSource || 'N/A'}</TableCell>
                                        <TableCell>{indexer.schedule || 'N/A'}</TableCell>
                                        <TableCell>{indexer.lastRunTime || 'N/A'}</TableCell>
                                        <TableCell>{indexer.nextRunTime || 'N/A'}</TableCell>
                                        <TableCell>{indexer.documentsProcessed || 'N/A'}</TableCell>
                                        <TableCell>
                                            <Badge variant={getBadgeVariant(indexer.status)}>
                                                {indexer.status || 'Unknown'}
                                            </Badge>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                </CardContent>
            </Card>

            {/* Data Sources Section */}
            <Card className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader>
                    <CardTitle className="text-lg font-medium">Available Data Sources</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="overflow-x-auto">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Name</TableHead>
                                    <TableHead>Type</TableHead>
                                    <TableHead>Container/Database</TableHead>
                                    <TableHead>Connection Status</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {report.dataSources?.map((dataSource) => (
                                    <TableRow key={dataSource.name} className="hover:bg-muted/50 transition-colors">
                                        <TableCell className="font-medium">{dataSource.name || 'N/A'}</TableCell>
                                        <TableCell>{dataSource.type || 'N/A'}</TableCell>
                                        <TableCell>{dataSource.container || 'N/A'}</TableCell>
                                        <TableCell>
                                            <Badge variant={getBadgeVariant(dataSource.connectionStatus)}>
                                                {dataSource.connectionStatus || 'Unknown'}
                                            </Badge>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};

export default IndexReportComponent;