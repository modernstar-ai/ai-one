export async function* consumeChunks(
  reader: ReadableStreamDefaultReader<Uint8Array | undefined>,
  decoder: TextDecoder
) {
  let partialChunk = '';
  while (true) {
    const { done, value } = await reader.read();

    partialChunk += decoder.decode(value, { stream: true });
    let belIndex;
    while ((belIndex = partialChunk.indexOf('')) > -1) {
      const completeChunk = partialChunk.slice(0, belIndex);
      partialChunk = partialChunk.slice(belIndex + 1);
      if (completeChunk) {
        try {
          const obj = JSON.parse(completeChunk);
          yield obj;
        } catch (e) {
          console.log('Chunk parsing error: ', e);
        }
      }
    }

    if (done) {
      break;
    }
  }
}
