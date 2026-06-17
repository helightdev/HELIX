import { docfxSource, getLLMText, getReferencePage, getReferenceRouteSlugs } from '@/lib/source';
import { referenceContentRoute } from '@/lib/shared';
import { notFound } from 'next/navigation';

export const revalidate = false;

function getReferenceMarkdownUrl(page: (typeof docfxSource)['$inferPage']) {
  const segments = [...getReferenceRouteSlugs(page), 'content.md'];

  return {
    segments,
    url: `${referenceContentRoute}/${segments.join('/')}`,
  };
}

export async function GET(_req: Request, { params }: RouteContext<'/llms.mdx/reference/[[...slug]]'>) {
  const { slug } = await params;
  const page = getReferencePage(slug?.slice(0, -1) ?? []);
  if (!page) notFound();

  return new Response(await getLLMText(page), {
    headers: {
      'Content-Type': 'text/markdown',
    },
  });
}

export function generateStaticParams() {
  return docfxSource.getPages().map((page) => ({
    slug: getReferenceMarkdownUrl(page).segments,
  }));
}
