import SimpleHeading from '@/components/Heading-Simple';

const HomePage = () => {
  return (
    <div className="flex h-screen bg-background text-foreground">

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading Title="Welcome to AgileChat" Subtitle="Accelerating your AI adoption" DocumentCount={0} />
      </div>
    </div>
  );
};

export default HomePage;
