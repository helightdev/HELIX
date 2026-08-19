import { source } from '@/lib/source';
import { DocsLayout } from 'fumadocs-ui/layouts/notebook';
import { baseOptions } from '@/lib/layout.shared';
import { LucideBlocks, LucideBook, LucideCodeXml, LucideLayers } from 'lucide-react';

export default function Layout({ children }: LayoutProps<'/docs'>) {
  const tree = source.getPageTree();

  return (
    <DocsLayout
        {...baseOptions()}
        tree={tree}
        tabs={[
            {
                title: 'Overview',
                url: '/docs',
                icon: <LucideBook className="size-full stroke-fd-primary" />,
                urls: new Set(['/docs', '/docs/prose', '/docs/previews', '/docs/getting-started'])
            },
            {
                title: 'Compose',
                description: 'Immediate Enough UI',
                url: '/docs/compose',
                icon: <LucideLayers className="size-full stroke-fd-primary" />,
                urls: new Set([
                    '/docs/compose',
                    '/docs/compose/components-and-generation',
                    '/docs/compose/prop-structs'
                ])
            },
            {
                title: 'Context',
                description: 'Mixins and Services',
                url: '/docs/context',
                icon: <LucideBlocks className="size-full stroke-fd-primary" />,
                urls: new Set([
                    '/docs/context',
                    '/docs/context/components-and-loading',
                    '/docs/context/scopes-and-dependencies',
                    '/docs/context/lifecycle',
                    '/docs/context/events',
                    '/docs/context/tickers-and-auto-dispose',
                    '/docs/context/mixin-expressions',
                    '/docs/context/mixin-values-and-predicates',
                    '/docs/context/mixin-control-flow',
                    '/docs/context/mixin-generation'
                ])
            },
            {
                title: 'Reference',
                description: 'C# Script Reference',
                url: '/reference',
                icon: <LucideCodeXml className="size-full stroke-sky-500" />
            }
        ]}
    >
      {children}
    </DocsLayout>
  );
}
