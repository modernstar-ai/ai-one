import SimpleHeading from '@/components/Heading-Simple';

const IndexesPage = () => {
  return (
    <div className="flex h-screen bg-background text-foreground">
      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading Title="Indexes" Subtitle="Manage your indexes" />
      </div>
    </div>
  );
};

export default IndexesPage;
