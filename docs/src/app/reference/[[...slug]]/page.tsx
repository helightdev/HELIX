import {docfxSource, getReferencePage, getReferenceRouteSlugs} from '@/lib/source';
import {
    DocsBody,
    DocsDescription,
    DocsPage,
    DocsTitle,
    MarkdownCopyButton,
    ViewOptionsPopover,
} from 'fumadocs-ui/layouts/notebook/page';
import {notFound} from 'next/navigation';
import type {Metadata} from 'next';
import {gitConfig, referenceContentRoute} from '@/lib/shared';
import {
    docfxUidToFileName,
    getItemSymbolKind,
    SymbolBadge,
    FRBreadcrumb,
    type FRPageData,
} from 'furef';

export const dynamicParams = false;

export default async function Page(props: PageProps<'/reference/[[...slug]]'>) {
    const params = await props.params;


    const page = getReferencePage(params.slug ?? []);
    if (!page) notFound();

    const markdownUrl = `${referenceContentRoute}/${[...getReferenceRouteSlugs(page), 'content.md'].join('/')}`;
    const githubUrl = page.data.docfx.uid
        ? `https://github.com/${gitConfig.user}/${gitConfig.repo}/blob/${gitConfig.branch}/docfx/${docfxUidToFileName(page.data.docfx.uid)}`
        : `https://github.com/${gitConfig.user}/${gitConfig.repo}/blob/${gitConfig.branch}/docfx/toc.yml`;

    return (
        <DocsPage
            toc={page.data.toc}
            full={page.data.full}
            breadcrumb={{
                component: <FRBreadcrumb slugs={page.slugs} title={page.data.title} source={docfxSource} />
            }}
        >
            <DocsTitle>{page.data.title}</DocsTitle>
            <DocsDescription className="mb-0">{page.data.description}</DocsDescription>
            <div className="flex flex-row gap-2 items-center border-b pb-6">
                <SymbolBadge kind={getItemSymbolKind(page.data.docfx.item)}/>
                <div className="grow"/>
                <MarkdownCopyButton markdownUrl={markdownUrl}/>
                <ViewOptionsPopover
                    markdownUrl={markdownUrl}
                    githubUrl={githubUrl}
                />
            </div>
            <DocsBody>
                <DocfxPage data={page.data}/>
            </DocsBody>
        </DocsPage>
    );
}

function DocfxPage({data}: { data: FRPageData }) {
    const Body = data.body;

    return <Body/>;
}

export function generateStaticParams() {
    return docfxSource.getPages().map((page) => ({
        slug: getReferenceRouteSlugs(page),
    }));
}

export async function generateMetadata(props: PageProps<'/reference/[[...slug]]'>): Promise<Metadata> {
    const params = await props.params;

    const page = getReferencePage(params.slug ?? []);
    if (!page) notFound();

    return {
        title: page.data.title,
        description: page.data.description,
    };
}
