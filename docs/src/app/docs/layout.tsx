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
                urls: new Set([
                    '/docs',
                    '/docs/prose',
                    '/docs/previews',
                    '/docs/getting-started',
                    '/docs/overview/mixins',
                    '/docs/overview/mixin-expressions',
                    '/docs/overview/mixin-values-and-predicates',
                    '/docs/overview/mixin-control-flow',
                    '/docs/overview/mixin-generation'
                ])
            },
            {
                title: 'Compose',
                description: 'Immediate Enough UI',
                url: '/docs/compose',
                icon: <LucideLayers className="size-full stroke-fd-primary" />,
                urls: new Set([
                    '/docs/compose',
                    '/docs/compose/compositions',
                    '/docs/compose/boundaries',
                    '/docs/compose/element-proxies',
                    '/docs/compose/prop-structs',
                    '/docs/compose/theming',
                    '/docs/compose/state-properties'
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
                    '/docs/context/scheduling',
                    '/docs/context/automatic-cleanup'
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
